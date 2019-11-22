﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using maxbl4.Infrastructure.Extensions.HttpContentExt;
using maxbl4.Infrastructure.Extensions.HttpClientExt;
using maxbl4.RaceLogic.Checkpoints;
using maxbl4.RaceLogic.Tests.CheckpointService.RfidSimulator;
using maxbl4.RfidDotNet;
using maxbl4.RfidDotNet.Infrastructure;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace maxbl4.RaceLogic.Tests.CheckpointService.Controllers
{
    public class CheckpointsControllerTests : IntegrationTestBase
    {
        public CheckpointsControllerTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }
        
        [Fact]
        public async Task HttpClient_should_wait_for_webhost_to_start()
        {
            // Make service slow on startup by putting 5000ms sleep
            var sw = Stopwatch.StartNew();
            using var svc = CreateRfidCheckpointService(5000);
            var client = new HttpClient();
            // Get data from service should succeed, but take 5000+ ms
            var response = await client.GetAsync("http://localhost:5000/cp");
            (await response.Content.ReadAsStringAsync()).ShouldBe("[]");
            sw.Stop();
            sw.ElapsedMilliseconds.ShouldBeGreaterThan(5000);
        }

        [Fact]
        public async Task Should_return_stored_checkpoints()
        {
            var ts = DateTime.UtcNow;
            WithStorageService(storageService =>
            {
                storageService.AppendCheckpoint(new Checkpoint("stored1", ts));
                storageService.AppendCheckpoint(new Checkpoint("stored2", ts.AddSeconds(100)));
            });
            
            using var svc = CreateRfidCheckpointService();
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:5000/cp");
            
            var checkpoints = JsonConvert.DeserializeObject<List<Checkpoint>>(await response.Content.ReadAsStringAsync());
            checkpoints.ShouldNotBeNull();
            checkpoints.Count(x => x.RiderId.StartsWith("stored")).ShouldBeGreaterThanOrEqualTo(2);
            checkpoints.ShouldContain(x => x.RiderId == "stored1");
            checkpoints.ShouldContain(x => x.RiderId == "stored2");
        }
        
        [Fact]
        public async Task Should_stream_checkpoints_over_websocket()
        {
            var tagListHandler = WithStorageService(storageService => new SimulatorBuilder(storageService).Build());
            
            using var svc = CreateRfidCheckpointService();
            tagListHandler.ReturnOnce(new Tag{TagId = "1"});
            tagListHandler.ReturnOnce(new Tag{TagId = "2"});
            var wsConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            await wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint cp) => checkpoints.Add(cp));
            tagListHandler.ReturnOnce(new Tag{TagId = "3"});
            tagListHandler.ReturnOnce(new Tag{TagId = "4"});
            await new Timing()
                .Logger(Logger)
                .FailureDetails(() => $"checkpoints.Count = {checkpoints.Count}")
                .ExpectAsync(() => checkpoints.Count >= 4);
        }
        
        [Fact]
        public async Task Should_append_manual_checkpoint()
        {
            var tagListHandler = WithStorageService(storageService => new SimulatorBuilder(storageService).Build());
            
            using var svc = CreateRfidCheckpointService();
            var wsConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/ws/cp")
                .Build();
            var checkpoints = new List<Checkpoint>();
            await wsConnection.StartAsync();
            await wsConnection.SendCoreAsync("Subscribe", new object[]{DateTime.UtcNow.AddHours(-1)});
            wsConnection.On("Checkpoint", (Checkpoint cp) => checkpoints.Add(cp));
            var cli = new HttpClient();
            (await cli.PutAsync("http://localhost:5000/cp", 
                new StringContent("\"555\"", Encoding.UTF8, "application/json"))).EnsureSuccessStatusCode();
            await new Timing()
                .Logger(Logger)
                .FailureDetails(() => $"checkpoints.Count = {checkpoints.Count}")
                .ExpectAsync(() => checkpoints.Count >= 1);
            checkpoints.ShouldContain(x => x.RiderId == "555");
        }
        
        [Fact]
        public async Task Should_remove_checkpoints()
        {
            var now = DateTime.UtcNow;
            var tagListHandler = WithStorageService(storageService =>
                {
                    storageService.AppendCheckpoint(new Checkpoint{Id = 1, RiderId = "111", Timestamp = now});
                    storageService.AppendCheckpoint(new Checkpoint{Id = 2, RiderId = "222", Timestamp = now.AddMinutes(1)});
                    storageService.AppendCheckpoint(new Checkpoint{Id = 3, RiderId = "333", Timestamp = now.AddMinutes(2)});
                    storageService.AppendCheckpoint(new Checkpoint{Id = 4, RiderId = "444", Timestamp = now.AddMinutes(3)});
                    storageService.AppendCheckpoint(new Checkpoint{Id = 5, RiderId = "555", Timestamp = now.AddMinutes(4)});
                    return new SimulatorBuilder(storageService).Build();
                });
            
            using var svc = CreateRfidCheckpointService();
            var cli = new HttpClient();
            
            var response = await cli.DeleteAsync("http://localhost:5000/cp/1");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).ShouldBe(1);
            var cps = await cli.GetAsync<List<Checkpoint>>("http://localhost:5000/cp");
            cps.Count.ShouldBe(4);
            cps.ShouldNotContain(x => x.Id == 1);
            
            response = await cli.DeleteAsync($"http://localhost:5000/cp?start={now.AddMinutes(1.5):u}&end={now.AddMinutes(3.5):u}");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).ShouldBe(2);
            cps = await cli.GetAsync<List<Checkpoint>>("http://localhost:5000/cp");
            cps.Count.ShouldBe(2);
            cps.ShouldNotContain(x => x.Id == 3 || x.Id == 4);
            
            response = await cli.DeleteAsync($"http://localhost:5000/cp");
            response.EnsureSuccessStatusCode();
            (await response.Content.ReadAs<int>()).ShouldBe(2);
            cps = await cli.GetAsync<List<Checkpoint>>("http://localhost:5000/cp");
            cps.Count.ShouldBe(0);
        }
    }
}
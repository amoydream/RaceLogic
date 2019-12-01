﻿using maxbl4.Race.CheckpointService.Services;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace maxbl4.Race.Tests.CheckpointService.Storage
{
    public class StorageOptionsTests
    {
        [Fact]
        public void ShouldSerializeAndDeserialize()
        {
            var options = new ServiceOptions
                {StorageConnectionString = "Filename=storage.litedb;InitialSize=123;UtcDate=true"};
            var s = JsonConvert.SerializeObject(options);
            var deserialized = JsonConvert.DeserializeObject<ServiceOptions>(s);
            deserialized.ShouldNotBeSameAs(options);
            deserialized.StorageConnectionString.ShouldBe("Filename=storage.litedb;InitialSize=123;UtcDate=true");
        }
    }
}
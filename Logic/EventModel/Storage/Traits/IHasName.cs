﻿namespace maxbl4.Race.Logic.EventStorage.Storage.Traits
{
    public interface IHasName : IHasTraits
    {
        string Name { get; set; }
        string Description { get; set; }
    }
}
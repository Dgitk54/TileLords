﻿using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace DataModel.Client
{
    public class MapAsRenderAbleChanged : IEvent
    {
        public Dictionary<PlusCode, MiniTile> Map { get; set; }
        public PlusCode Location { get; set; }

        public override string ToString()
        {
            var nullval = Map.Values;
            var nullcount = 0;
            var nonnull = 0;
            foreach (var v in nullval)
            {
                if (v == null)
                {
                    nullcount++;
                }
                else
                {
                    nonnull++;
                }
            }

            return "MapAsRenderAbleChanged, has null values:" + nullcount + "    NONNULL:" + nonnull;
        }
    }
}
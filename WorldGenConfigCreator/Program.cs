﻿using Newtonsoft.Json;
using System;
using System.IO;
using DataModel.Common.BiomeConfigs;
namespace WorldGenConfigCreator
{
    class Program
    {
        static void Main(string[] args)
        {

            GrasslandBiome grass = new GrasslandBiome();
            DesertBiome desert = new DesertBiome();
            JungleBiome jungle = new JungleBiome();
            MountainsBiome mountains = new MountainsBiome();
            SavannaBiome savanna = new SavannaBiome();
            SnowBiome snow = new SnowBiome();
            SwampBiome swamp = new SwampBiome();
            WaterBodyBiome water = new WaterBodyBiome();
            string workingDirectory = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            Console.WriteLine(projectDir);
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\Grassland.json", JsonConvert.SerializeObject(grass, Formatting.Indented));
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\Desert.json", JsonConvert.SerializeObject(desert, Formatting.Indented));
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\Jungle.json", JsonConvert.SerializeObject(jungle, Formatting.Indented));
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\Mountains.json", JsonConvert.SerializeObject(mountains, Formatting.Indented));
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\Savanna.json", JsonConvert.SerializeObject(savanna, Formatting.Indented));
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\Snow.json", JsonConvert.SerializeObject(snow, Formatting.Indented));
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\Swamp.json", JsonConvert.SerializeObject(swamp, Formatting.Indented));
            File.WriteAllText(projectDir + @"\DataModel.Common\BiomeConfigs\WaterBody.json", JsonConvert.SerializeObject(water, Formatting.Indented));


        }
    }
}

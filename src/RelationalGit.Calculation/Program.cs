using CsvHelper;
using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using RelationalGit.Data;

namespace RelationalGit.Calculation
{
    class Program
    {
        static void Main(string[] args)
        {
            var actualId = 38;
            var simulationsIds = new int[] { 39,40,41,42,43,44,45};
            var path = @"Results\kubernetes_accuracy";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //CalculateWorkloadRaw(simulationsIds,10,path);
            //CalculateFaRRaw(simulationsIds, path);
            //CalculateTotalFaRRaw(simulationsIds, path);
            //CalculateExpertiseRaw(simulationsIds, path);

        }


    }
}

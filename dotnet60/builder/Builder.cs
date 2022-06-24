using Builder.Utility;
using NugetWorker;
using System;
using System.IO;
using Builder.Engine;
using Fission.Common;

namespace Builder
{
    class Builder
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting builder task");
            var logFileName = $"{DateTime.Now.ToString("yyyy_MM_dd")}_{Guid.NewGuid().ToString()}.log";
                        
            try
            {
                string _logdirectory = CompilerHelper.Instance.builderSettings.BuildLogDirectory;
                BuilderHelper.Instance._logFileName = Path.Combine(_logdirectory, logFileName);
                BuilderHelper.Instance.logger = new Utility.Logger(
                           BuilderHelper.Instance._logFileName);

                NugetHelper.Instance.logger = new NugetWorker.Logger(BuilderHelper.Instance._logFileName);

                Console.WriteLine($"detailed logs for this build will be at: {Path.Combine(_logdirectory, logFileName)}");


                BuilderEngine builderEngine = new BuilderEngine();
                builderEngine.BuildPackage().Wait();
            }
            catch (Exception ex)
            {
                string detailedException = string.Empty;
                try
                {
                    detailedException= BuilderHelper.Instance.DeepException(ex);
                    Console.WriteLine($"Exception during build: {Environment.NewLine} {ex.Message} | {ex.StackTrace} | {Environment.NewLine} {detailedException}");
                }
                catch(Exception childEx)
                {
                    Console.WriteLine($"{Environment.NewLine} Exception during build:{ex.Message} |{Environment.NewLine}  {ex.StackTrace} {Environment.NewLine} ");
                }              

                throw;
            }

            Console.WriteLine("Builder task done");
        }
    }
}

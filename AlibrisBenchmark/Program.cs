using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AlibrisBenchmark
{
    class Program
    {

        private const string BASE_URL = "http://partnersearch.alibris.com/cgi-bin/";

        private static HttpClient _client;

        private static HttpClient httpClient
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.BaseAddress = new Uri(BASE_URL);
                    _client.DefaultRequestHeaders.Accept.Clear();
                    _client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("text/xml"));


                }
                return _client;

            }
        }


        static void Main(string[] args)
        {
            int numAlabrisCalls = 0;
            int msAlabrisCalls = 0;
            var lockObj = new Object();

            var threadCount = 10;


          

            Console.Write("Enter Alibris Site Id#:");

            var site = Console.ReadLine();

            site = site.Replace("\n", "");

            string[] lines = System.IO.File.ReadAllLines("isbns.txt");
            DateTime batchStart = DateTime.Now;
            Console.WriteLine($"Starting Benchmark with {threadCount} threads");


            Parallel.ForEach(
    lines,
    new ParallelOptions { MaxDegreeOfParallelism = threadCount },
    line =>
    {


        var startTime = DateTime.Now;
     


                var resp = httpClient.GetStreamAsync($"search?site={site}&qisbn={line}&qsort=p&chunk=1");

        resp.Wait();


        var ms = (int)(DateTime.Now - startTime).TotalMilliseconds;

      
        lock (lockObj)
        {
            msAlabrisCalls += ms;
            numAlabrisCalls++;
            if (numAlabrisCalls % 100 == 0)
            {
                        Console.WriteLine($"Alabris calls = {numAlabrisCalls} avg ms = {msAlabrisCalls / numAlabrisCalls}");
            }
           
        }


        if (resp.Status != System.Threading.Tasks.TaskStatus.RanToCompletion)
        {
            Console.WriteLine(resp.Exception);
            throw resp.Exception;
        }

        var xml = resp.Result;

        xml.Dispose();

        resp.Dispose();




    });

            var msTotal = (int)(DateTime.Now - batchStart).TotalMilliseconds;

            Console.WriteLine($"Alabris calls = {numAlabrisCalls} total time = {Math.Round((decimal)msTotal / (decimal)1000,2)} secs = { Math.Round((decimal)numAlabrisCalls / ((decimal)msTotal / (decimal)1000),2) }/sec ({ Math.Round((decimal)numAlabrisCalls / ((decimal)msTotal / (decimal)1000) * 3600,0) }/hr)");

            Console.ReadLine();

        }
    }
}

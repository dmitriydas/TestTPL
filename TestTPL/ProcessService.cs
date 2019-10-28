using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestTPL
{
    public class ProcessService
    {
        public static Result ExecuteAsync1(MyData myData)
        {
            //try
            //{
                WebClient web = new WebClient();
                TaskCompletionSource<Result> res = new TaskCompletionSource<Result>();
                Task task = Task<Result>.Run(() => web.DownloadStringAsync(new Uri("http://localhost:49182/Slow.ashx")));
                task.Wait();
                Console.WriteLine($"Data = {myData}");
                if (myData != null && myData.Id % 9 == 0)
                    throw new Exception("Test");
                return Result.Ok();
            //}
            //catch (Exception ex)
            //{
            //    return Result.Failure($"Exception: {ex.Message}");
            //}
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AccessSystem_IB.Data.database
{
    public class DbContext
    {
        private Thread WorkingThread { get; } = new Thread(async () =>
        {
            while (Instance!=null)
            {
                while (Funcs.Count>0)
                {
                   var func =  Funcs.Peek();
                   await using ApplicationContext context = new ApplicationContext();
                   await func.Invoke(context);
                   context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                   Funcs.Dequeue();
                }
                Thread.Sleep(1);
            }
        });

        public delegate Task Func(ApplicationContext dbContext);
        private static Queue<Func> Funcs { get; } = new Queue<Func>();

        private DbContext() => WorkingThread.Start();
        private static DbContext Instance { get; set; }
        public static DbContext GetInstance() => Instance ??= new DbContext();

        public void Send(Func func)
        {
            Funcs.Enqueue(func);
        }

        public async Task SendAsync(Func func)
        {
            Funcs.Enqueue(func);
            await Task.Run(()=>{
                while (Funcs.Contains(func))
                {

                }});
        }
    }
}
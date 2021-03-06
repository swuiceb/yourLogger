using System;
using System.Linq;
using System.Threading.Tasks;
using yourLogs.Exceptions.Core.LogWriters.Providers;
using yourLogs.Exceptions.Core.Models;
using yourLogs.Exceptions.Db.Ef.LogWriters.RollingDb;
using yourLogs.Exceptions.Db.Ef.Repository;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace yourLogs.Exceptions.Db.Ef.Tests.LogWriters
{
    [TestFixture]
    public class RollingDbTests
    {
        private readonly DbContextOptions inMemoryOption;
        private readonly ErrorDbContext context;

        public RollingDbTests()
        {
            inMemoryOption =
                new DbContextOptionsBuilder()
                    .UseInMemoryDatabase("RollingDbTests")
                    .Options;

            context = new ErrorDbContext(inMemoryOption);
        }

        [SetUp]
        public void Setup()
        {
            context.Logs.RemoveRange(context.Logs);
            context.SaveChanges();
        }

        [Test]
        public async Task InsertError_EnsureLog()
        {
            var repository = new EfCoreInMemRepository(() => new ErrorDbContext(inMemoryOption));
            var handler = new RollingDbLogWriter(5,
                repository, ExceptionTextProviders.Default,
                codeProvider: (ex) => ex.Message);

            await handler.LogErrorAsync(new Exception("hi"));
            await handler.LogErrorAsync(new Exception("hi"));
            
            Assert.AreEqual(0, context.Logs.Count());
            
            await handler.LogErrorAsync(new Exception("hi"));
            await handler.LogErrorAsync(new Exception("hi"));
            await handler.LogErrorAsync(new Exception("hi"));
            
            Assert.AreEqual(5, context.Logs.Count());
        }
        
        [Test]
        public async Task InsertWarning_AndErrors_EnsureLog()
        {
            var repository = new EfCoreInMemRepository(() => new ErrorDbContext(inMemoryOption));
            var handler = new RollingDbLogWriter(5,
                repository, ExceptionTextProviders.Default,
                codeProvider: (ex) => ex.Message);

            await handler.LogWarnAsync(new Exception("hi"));
            await handler.LogWarnAsync(new Exception("hi"));
            
            Assert.AreEqual(0, context.Logs.Count());
            
            await handler.LogWarnAsync(new Exception("hi"));
            await handler.LogErrorAsync(new Exception("hi"));
            await handler.LogErrorAsync(new Exception("hi"));
            
            Assert.AreEqual(5, context.Logs.Count());
        }
    }
}
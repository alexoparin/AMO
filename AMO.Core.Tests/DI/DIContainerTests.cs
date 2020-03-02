using System;
using AMO.Core.DI;
using AMO.Core.Infrastructure;
using DbImport2.Core.Tests.DI.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbImport2.Core.Tests.DI
{
    [TestClass]
    public class IoCContainerTests
    {
        private IDIContainer _container;

        [TestInitialize]
        public void TestInitialize()
        {
            _container = new DIContainer();
            SomeTypeImpl.Reset();
        }

        [TestMethod]
        public void TypeResolveBasicTest()
        {
            _container.RegisterType(typeof(ISomeType), typeof(SomeTypeImpl), LifetimeManagement.Transient);
            var instance = _container.Resolve(typeof(ISomeType));

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(SomeTypeImpl));
            Assert.AreEqual(1, SomeTypeImpl.CreatedCount);
        }

        [TestMethod]
        public void InstanceResolveBasicTest()
        {
            var instance = new SomeTypeImpl();
            _container.RegisterInstance(typeof(ISomeType), instance);

            Assert.AreEqual(instance, _container.Resolve(typeof(ISomeType)));
        }

        [TestMethod]
        public void FactoryResolveBasicTest()
        {
            //_container.RegisterFactory(typeof(ISomeType), 
            //    ref IDIResolveContext ctx => new SomeTypeImpl());

            //var instance = _container.Resolve(typeof(ISomeType));

            //Assert.IsNotNull(instance);
            //Assert.IsInstanceOfType(instance, typeof(SomeTypeImpl));
            //Assert.AreEqual(1, SomeTypeImpl.CreatedCount);
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TypeResolveTransientTest()
        {
            _container.RegisterType(typeof(ISomeType), typeof(SomeTypeImpl), LifetimeManagement.Transient);
            var instance1 = _container.Resolve(typeof(ISomeType));
            var instance2 = _container.Resolve(typeof(ISomeType));

            Assert.IsNotNull(instance1);
            Assert.IsNotNull(instance2);
            Assert.AreNotEqual(instance1, instance2);
            Assert.AreEqual(2, SomeTypeImpl.CreatedCount);
        }

        [TestMethod]
        public void TypeResolveSingletonTest()
        {
            _container.RegisterType(typeof(ISomeType), typeof(SomeTypeImpl), LifetimeManagement.Singleton);
            var instance1 = _container.Resolve(typeof(ISomeType));
            var instance2 = _container.Resolve(typeof(ISomeType));

            Assert.IsNotNull(instance1);
            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(1, SomeTypeImpl.CreatedCount);
        }
    }
}

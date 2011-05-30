﻿using System;
using MbCache.Configuration;
using MbCache.Core;
using MbCacheTest.CacheForTest;
using MbCacheTest.TestData;
using NUnit.Framework;
using SharpTestsEx;

namespace MbCacheTest.Logic
{
    [TestFixture]
    public class InvalidateCacheTest
    {
        private IMbCacheFactory factory;

        [SetUp]
        public void Setup()
        {
            var builder = new CacheBuilder(ConfigurationData.ProxyFactory, new TestCache(), new ToStringMbCacheKey());

            builder
                .For<ObjectReturningNewGuids>()
                .CacheMethod(c => c.CachedMethod())
                .CacheMethod(c => c.CachedMethod2())
                .As<IObjectReturningNewGuids>();

			builder
				.For<ObjectWithParametersOnCachedMethod>()
				.CacheMethod(c => c.CachedMethod(null))
				.As<IObjectWithParametersOnCachedMethod>();

            factory = builder.BuildFactory();
        }


        [Test]
        public void VerifyInvalidateByType()
        {
            var obj = factory.Create<IObjectReturningNewGuids>();
            var value1 = obj.CachedMethod();
            var value2 = obj.CachedMethod2();
            Assert.AreEqual(value1, obj.CachedMethod());
            Assert.AreEqual(value2, obj.CachedMethod2());
            Assert.AreNotEqual(value1, value2);
            factory.Invalidate<IObjectReturningNewGuids>();
            Assert.AreNotEqual(value1, obj.CachedMethod());
            Assert.AreNotEqual(value2, obj.CachedMethod2());
        }

        [Test]
        public void InvalidatingNonCachingComponentThrows()
        {
            Assert.Throws<ArgumentException>(() => factory.Invalidate(3));
        }

        [Test]
        public void InvalidatingNonCachingComponentAndMethodThrows()
        {
            Assert.Throws<ArgumentException>(() => factory.Invalidate(3, theInt => theInt.CompareTo(44)));
        }


		[Test]
		public void VerifyInvalidateByInstance()
		{
			var obj = factory.Create<IObjectReturningNewGuids>();
			var obj2 = factory.Create<IObjectReturningNewGuids>();
			var value1 = obj.CachedMethod();
			var value2 = obj2.CachedMethod2();
			Assert.AreEqual(value1, obj.CachedMethod());
			Assert.AreEqual(value2, obj.CachedMethod2());
			Assert.AreNotEqual(value1, value2);
			factory.Invalidate(obj);
			Assert.AreNotEqual(value1, obj.CachedMethod());
			Assert.AreNotEqual(value2, obj.CachedMethod2());
		}


        [Test]
        public void InvalidateByCachingComponent()
        {
            var obj = factory.Create<IObjectReturningNewGuids>();
            var value = obj.CachedMethod();

            ((ICachingComponent) obj).Invalidate();
            Assert.AreNotEqual(value, obj.CachedMethod());
        }

        [Test]
        public void InvalidateSpecificMethod()
        {
            var obj = factory.Create<IObjectReturningNewGuids>();
            var value1 = obj.CachedMethod();
            var value2 = obj.CachedMethod2();
            Assert.AreEqual(value1, obj.CachedMethod());
            Assert.AreEqual(value2, obj.CachedMethod2());
            Assert.AreNotEqual(value1, value2);
            factory.Invalidate(obj, method => obj.CachedMethod());
            Assert.AreNotEqual(value1, obj.CachedMethod());
            Assert.AreEqual(value2, obj.CachedMethod2());
		}

		[Test]
		public void InvalidateSpecificMethodWithSpecificParameter()
		{
			var obj = factory.Create<IObjectWithParametersOnCachedMethod>();
			var value1 = obj.CachedMethod("roger");
			var value2 = obj.CachedMethod("moore");
			factory.Invalidate(obj, method => method.CachedMethod("roger"), true);

			value1.Should().Not.Be.EqualTo(obj.CachedMethod("roger"));
			value2.Should().Be.EqualTo(obj.CachedMethod("moore"));
		}

		[Test]
		public void InvalidateSpecificMethodWithSpecificParameterNotUsed()
		{
			var obj = factory.Create<IObjectWithParametersOnCachedMethod>();
			var value1 = obj.CachedMethod("roger");
			var value2 = obj.CachedMethod("moore");
			factory.Invalidate(obj, method => method.CachedMethod("roger"), false);

			value1.Should().Not.Be.EqualTo(obj.CachedMethod("roger"));
			value2.Should().Not.Be.EqualTo(obj.CachedMethod("moore"));
		}
	}
}

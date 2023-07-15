/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Java.Util;
using NUnit.Framework;

using Test4giis.Qacoverapp;

namespace Test4giis.Qacover.Model
{
	public class TestUtil
	{
		protected internal Variability variant = new Variability("sqlite");

		// This does not inherits because there is no bd, but variability must be defned
		// ensures clean start without custom options
		[NUnit.Framework.SetUp]
		public virtual void SetUp()
		{
			Configuration.GetInstance().Reset();
		}

		[NUnit.Framework.TearDown]
		public virtual void TearDown()
		{
			Configuration.GetInstance().Reset();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestStackTrace()
		{
			AppSimpleJdbc app = new AppSimpleJdbc(variant);
			StackLocator stack = app.MyGetStackTraceTargetMethod();
			NUnit.Framework.Assert.AreEqual("test4giis.qacoverapp.appsimplejdbc", stack.GetClassName().ToLower());
			NUnit.Framework.Assert.AreEqual("mygetstacktracetargetmethod", stack.GetMethodName().ToLower());
			// approximate location of the line
			NUnit.Framework.Assert.IsTrue(stack.GetLineNumber() > 10);
			NUnit.Framework.Assert.IsTrue(stack.GetLineNumber() < 50);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestStackTraceEmpty()
		{
			// excludes all packages to achieve an empty call stack
			Configuration.GetInstance().AddStackExclusion("test4giis").AddStackExclusion("giis").AddStackExclusion("java.").AddStackExclusion("junit.").AddStackExclusion("sun.").AddStackExclusion("org.").AddStackExclusion("microsoft.").AddStackExclusion("nunit.");
			AppSimpleJdbc app = new AppSimpleJdbc(variant);
			StackLocator stack = app.MyGetStackTraceTargetMethod();
			NUnit.Framework.Assert.AreEqual("undefined", stack.GetClassName());
			NUnit.Framework.Assert.AreEqual("undefined", stack.GetMethodName());
			NUnit.Framework.Assert.AreEqual(0, stack.GetLineNumber());
		}

		public virtual void TestClassSummaryFileOrder()
		{
		}

		// Accessing properties files
		[Test]
		public virtual void TestProperties()
		{
			// qacover.properties in the project rootraiz del proyecto
			Configuration opt = Configuration.GetInstance();
			Properties prop = opt.GetProperties("qacover.properties");
			NUnit.Framework.Assert.AreEqual("false", prop.GetProperty("qacover.query.infer.parameters"));
			// fortest.properties in src/test/resources, java only
			if (new Variability().IsJava())
			{
				prop = opt.GetProperties("fortest.properties");
				NUnit.Framework.Assert.AreEqual("X Y Z", prop.GetProperty("property.one"));
			}
			// Not existing
			try
			{
				prop = opt.GetProperties("noexiste.properties");
				NUnit.Framework.Assert.Fail("se esperaba excepction");
			}
			catch (Exception)
			{
			}
		}
		// pass
	}
}

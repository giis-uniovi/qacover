/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Giis.Qacover.Core.Services;
using Giis.Qacover.Model;
using Java.Util;
using NUnit.Framework;

using Test4giis.Qacover;
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
			Base.ConfigureTestOptions();
		}

		[NUnit.Framework.TearDown]
		public virtual void TearDown()
		{
			Base.ConfigureTestOptions();
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestStackTrace()
		{
			AppSimpleJdbc app = new AppSimpleJdbc(variant);
			StackLocator stack = app.MyGetStackTraceTargetMethod();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("test4giis.qacoverapp.appsimplejdbc", stack.GetClassName().ToLower());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("mygetstacktracetargetmethod", stack.GetMethodName().ToLower());
			// approximate location of the line
			NUnit.Framework.Legacy.ClassicAssert.IsTrue(stack.GetLineNumber() > 10);
			NUnit.Framework.Legacy.ClassicAssert.IsTrue(stack.GetLineNumber() < 50);
		}

		/// <exception cref="Java.Sql.SQLException"/>
		[Test]
		public virtual void TestStackTraceEmpty()
		{
			// excludes all packages to achieve an empty call stack
			Configuration.GetInstance().AddStackExclusion("test4giis").AddStackExclusion("giis").AddStackExclusion("java.").AddStackExclusion("junit.").AddStackExclusion("sun.").AddStackExclusion("org.").AddStackExclusion("microsoft.").AddStackExclusion("nunit.");
			AppSimpleJdbc app = new AppSimpleJdbc(variant);
			StackLocator stack = app.MyGetStackTraceTargetMethod();
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("undefined", stack.GetClassName());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("undefined", stack.GetMethodName());
			NUnit.Framework.Legacy.ClassicAssert.AreEqual(0, stack.GetLineNumber());
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
			NUnit.Framework.Legacy.ClassicAssert.AreEqual("false", prop.GetProperty("qacover.query.infer.parameters"));
			// fortest.properties in src/test/resources, java only
			if (new Variability().IsJava())
			{
				prop = opt.GetProperties("fortest.properties");
				NUnit.Framework.Legacy.ClassicAssert.AreEqual("X Y Z", prop.GetProperty("property.one"));
			}
			// Not existing
			try
			{
				prop = opt.GetProperties("noexiste.properties");
				NUnit.Framework.Legacy.ClassicAssert.Fail("se esperaba excepction");
			}
			catch (Exception)
			{
			}
		}
		// pass
	}
}

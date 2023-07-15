using System;

namespace Giis.Qacover.Portable
{
	public class Jdk14PropertiesFactory
	{
        public virtual Java.Util.Properties GetPropertiesFromSystemProperty(bool isJava14, string fileName, string defaultFileName)
        {
            throw new Exception("PropertiesFactory.GetPropertiesFromSystemProperty not supported in .NET");
        }
    }
}

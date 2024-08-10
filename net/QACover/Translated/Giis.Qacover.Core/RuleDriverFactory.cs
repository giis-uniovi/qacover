/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////


namespace Giis.Qacover.Core
{
	public class RuleDriverFactory
	{
		/// <summary>
		/// Instantiation of a rule driver according with the current configuration
		/// options.
		/// </summary>
		/// <remarks>
		/// Instantiation of a rule driver according with the current configuration
		/// options. At this moment, only fpc
		/// </remarks>
		public virtual RuleDriver GetDriver()
		{
			return new RuleDriverFpc();
		}
	}
}

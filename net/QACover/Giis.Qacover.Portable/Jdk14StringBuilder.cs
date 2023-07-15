using System.Text;


namespace Giis.Qacover.Portable
{
	/// <summary>
	/// Utilizar esta clase cuando se va a hacer downgrade de codigo a jdk 1.4, ya que no tiene StringBuilder:
	/// El codigo con el downgrade implmentara otra version de esta clase.
	/// </summary>
	public class Jdk14StringBuilder
	{
		internal StringBuilder s = new StringBuilder();

		public virtual int Length()
		{
			return s.Length;
		}

		public virtual void Append(string str)
		{
			s.Append(str);
		}

		public override string ToString()
		{
			return s.ToString();
		}
	}
}

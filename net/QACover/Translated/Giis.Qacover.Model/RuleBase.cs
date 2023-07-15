/////////////////////////////////////////////////////////////////////////////////////////////
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////
/////////////////////////////////////////////////////////////////////////////////////////////


namespace Giis.Qacover.Model
{
	/// <summary>
	/// Base class to store the standard results/indicators of the evaluation of
	/// (1) individual rules and (2) aggregated information of the evaluation of queries
	/// </summary>
	public abstract class RuleBase
	{
		protected internal const string Rdead = "dead";

		protected internal const string Rcount = "count";

		protected internal const string Rerror = "error";

		public const string TagError = "error";

		// Covered
		// The indicator are stored as attributes, the implementatin
		// is specific to the subclass (query or rule)
		protected internal abstract string GetAttribute(string name);

		protected internal abstract void SetAttribute(string name, string value);

		public override string ToString()
		{
			return "count=" + GetCount() + ",dead=" + GetDead() + (this.GetError() > 0 ? ",error=" + GetError() : string.Empty);
		}

		public virtual void Reset()
		{
			SetCount(0);
			SetDead(0);
			SetError(0);
		}

		protected internal virtual int GetIntAttribute(string name)
		{
			string current = GetAttribute(name);
			if (current == null || string.Empty.Equals(current))
			{
				// no existe el atributo
				return 0;
			}
			else
			{
				return int.Parse(current);
			}
		}

		protected internal virtual void IncrementIntAttribute(string name, int value)
		{
			string current = GetAttribute(name);
			if (current == null || string.Empty.Equals(current))
			{
				// no existe el atributo, pone el valor
				SetAttribute(name, value.ToString());
			}
			else
			{
				// existe, incrementa
				SetAttribute(name, (value + System.Convert.ToInt32(GetAttribute(name))).ToString());
			}
		}

		public virtual int GetCount()
		{
			return GetIntAttribute(Rcount);
		}

		public virtual void SetCount(int count)
		{
			SetAttribute(Rcount, count.ToString());
		}

		public virtual void AddCount(int value)
		{
			IncrementIntAttribute(Rcount, value);
		}

		public virtual int GetDead()
		{
			return GetIntAttribute(Rdead);
		}

		public virtual void SetDead(int dead)
		{
			SetAttribute(Rdead, dead.ToString());
		}

		public virtual void AddDead(int value)
		{
			IncrementIntAttribute(Rdead, value);
		}

		public virtual int GetError()
		{
			return GetIntAttribute(Rerror);
		}

		public virtual void SetError(int error)
		{
			if (GetError() > 0)
			{
				// evita este atributo si no hay error por ser excepcional (nunca decrementara hasta cero)
				SetAttribute(Rerror, error.ToString());
			}
		}

		public virtual void AddError(int value)
		{
			IncrementIntAttribute(Rerror, value);
		}
	}
}

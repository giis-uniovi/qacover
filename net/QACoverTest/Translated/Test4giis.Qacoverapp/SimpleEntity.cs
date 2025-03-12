using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
/////// THIS FILE HAS BEEN AUTOMATICALLY CONVERTED FROM THE JAVA SOURCES. DO NOT EDIT ///////

namespace Test4giis.Qacoverapp
{
    /// <summary>
    /// Used in test for Apache commons DbUtils
    /// </summary>
    public class SimpleEntity
    {
        private int id;
        private int num;
        private string text;
        public virtual int GetId()
        {
            return this.id;
        }

        public virtual int GetNum()
        {
            return this.num;
        }

        public virtual string GetText()
        {
            return this.text;
        }

        public virtual void SetId(int value)
        {
            this.id = value;
        }

        public virtual void SetNum(int value)
        {
            this.num = value;
        }

        public virtual void SetText(string value)
        {
            this.text = value;
        }
    }
}
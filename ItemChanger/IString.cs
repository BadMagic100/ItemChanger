﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemChanger.Extensions;

namespace ItemChanger
{
    public interface IString
    {
        string Value { get; }
        IString Clone();
    }

    public class LanguageString : IString
    {
        public string key;
        public string sheet;

        [Newtonsoft.Json.JsonIgnore]
        public string Value => Language.Language.Get(key, sheet)?.Replace("<br>", "\n");
        public IString Clone() => (IString)MemberwiseClone();
    }

    public class BoxedString : IString
    {
        public string Value { get; set; }
        public IString Clone() => (IString)MemberwiseClone();
    }

    public class PaywallString : IString
    {
        public string key;
        public string sheet;

        [Newtonsoft.Json.JsonIgnore]
        public string Value => Language.Language.Get(key, sheet)?.Replace("<br>", "\n").CapLength(125) 
            + "...\nYou have reached your monthly free article limit. To continue reading, subscribe now with this limited time offer!";
        public IString Clone() => (IString)MemberwiseClone();
    }

}
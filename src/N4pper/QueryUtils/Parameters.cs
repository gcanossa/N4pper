using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N4pper.QueryUtils
{
    public class Parameters
    {
        public IList<string> Mappings { get; protected set; }
        public string Suffix { get; protected set; }
        public Parameters(IEnumerable<string> props, string suffix=null)
        {
            props = props ?? throw new ArgumentNullException(nameof(props));

            Mappings = props.ToList();
            Suffix = suffix ?? "";
        }

        public void Apply(IEntity entity)
        {
            foreach (string key in Mappings)
            {
                if (entity.Props.ContainsKey(key))
                    entity.Props[key] = new Parameter($"{key}{Suffix}");
            }
        }

        public Dictionary<string, object> Prepare(Dictionary<string, object> original)
        {
            original = original ?? new Dictionary<string, object>();
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (string key in Mappings)
            {
                if (original.ContainsKey(key))
                    values.Add($"{key}{Suffix}", original[key]);
            }

            return values;
        }
    }
}

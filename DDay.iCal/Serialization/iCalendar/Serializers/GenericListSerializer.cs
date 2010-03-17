﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace DDay.iCal.Serialization.iCalendar
{
    public class GenericListSerializer :
        SerializerBase
    {
        #region Private Fields

        Type _InnerType;
        Type _ObjectType;

        #endregion

        #region Constructors

        public GenericListSerializer(Type objectType)
        {
            _InnerType = objectType.GetGenericArguments()[0];
            
            Type listDef = typeof(List<>);
            _ObjectType = listDef.MakeGenericType(typeof(object));
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return _ObjectType; }
        }

        public override string SerializeToString(object obj)
        {
            // NOTE: this behavior is implemented in the PropertySerializer.
            throw new NotImplementedException();
        }

        public override object Deserialize(TextReader tr)
        {
            ICalendarProperty p = SerializationContext.Peek() as ICalendarProperty;
            if (p != null)
            {
                // Get a serializer factory to deserialize the contents of this list
                ISerializerFactory sf = GetService<ISerializerFactory>();

                object listObj = Activator.CreateInstance(_ObjectType);
                if (listObj != null)
                {
                    // Get a serializer for the inner type
                    IStringSerializer stringSerializer = sf.Build(_InnerType, SerializationContext) as IStringSerializer;
                    if (stringSerializer != null)
                    {
                        // Deserialize the inner object
                        string value = tr.ReadToEnd();
                        object objToAdd = stringSerializer.Deserialize(new StringReader(value));

                        // If deserialization failed, pass the string value
                        // into the list.
                        if (objToAdd == null)
                            objToAdd = value;

                        if (objToAdd != null)
                        {
                            // FIXME: cache this
                            MethodInfo mi = _ObjectType.GetMethod("Add");
                            if (mi != null)
                            {
                                // Add the object to the list
                                mi.Invoke(listObj, new object[] { objToAdd });

                                return listObj;
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion
    }
}

using System;
using System.Data.Common;
using System.Reflection;

namespace Giis.Qacover.Driver
{
    /**
     * Es invocado por el wrapper de la conexion antes de ejecutar una sql,
     * y se encarga de invocar al event listener mediante reflexion.
     * Se ha de configurar la variable listenerClassName con la clase que actua de listener.
     * Hay dos formas de configurarlo:
     * -mediante la variable de entorno QACOVER_LISTENER_CLASS (para ejecucion en web o docker)
     * -modificando directamente la variable estatica (para ejecucion en consola o tests)
     */
    public class EventTrigger
    {
        //Definir la listener class como Giis.Qacover.Driver.EventListener
        private static string listenerClassName = null;
        static EventTrigger() {
            string listener = System.Environment.GetEnvironmentVariable("QACOVER_LISTENER_CLASS");
            if (!string.IsNullOrEmpty(listener))
                listenerClassName = listener;
        }

        public EventTrigger() { }
        public static void SetListenerClassName(string className)
        {
            listenerClassName = className;
        }
        public void InvokeListener(DbConnection db, string sql, DbParameterCollection parameters)
        {
            if (String.IsNullOrEmpty(listenerClassName)) //no hay configuracion, finaliza
                return;
            //Obtiene la clase y el metodo
            Type targetType = Type.GetType(listenerClassName);
            ConstructorInfo targetConstructor = targetType.GetConstructor(Type.EmptyTypes);
            object magicClassObject = targetConstructor.Invoke(new object[] { });
            MethodInfo targetMethod = targetType.GetMethod("OnBeforeExecuteQuery");
            object targetValue = targetMethod.Invoke(magicClassObject, new object[] { db, sql, parameters });
        }
    }
}

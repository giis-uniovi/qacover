using Microsoft.EntityFrameworkCore;

namespace Giis.Qacover.Ef2driver

{
    /**
     * Contexto a utilizar como base para interceptar sql
     * (sutituye al anterior que usaba un componente no nativo Z.EntityFramework...)
     * Se trata de una prueba de concepto basica usado de la siguiente forma:
     * - El contexto de la aplicacion heredara de este
     * - En OnConfiguring invocara al correspondiente metodo 
     *   de su base (esta clase) para instalar el interceptor
     * - Cuando se usa la bd durante tests puede deshabilitarse 
     *   para evitar evaluar cobertura de queries que no forman parte de la aplicacion
     */

    public class Ef2InterceptorContext : DbContext
    {
        private readonly Ef2EventListener CommandInterceptor = new();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options);
            options.AddInterceptors(CommandInterceptor);
        }

        public virtual Ef2InterceptorContext DisableInterceptor()
        {
            CommandInterceptor.Enabled=false;
            return this;
        }

    }
}

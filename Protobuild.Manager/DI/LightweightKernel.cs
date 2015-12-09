using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protobuild.Manager
{
    public class LightweightKernel
    {
        private Dictionary<Type, Type> m_Bindings = new Dictionary<Type, Type>();

        private Dictionary<string, Type> m_NamedBindings = new Dictionary<string, Type>();

        private Dictionary<Type, bool> m_KeepInstance = new Dictionary<Type, bool>();

        private Dictionary<Type, object> m_Instances = new Dictionary<Type, object>();

        public void Bind<TInterface, TImplementation>() where TImplementation : TInterface
        {
            this.m_Bindings.Add(typeof(TInterface), typeof(TImplementation));
            this.m_KeepInstance.Add(typeof(TInterface), false);
        }

        public void BindAndKeepInstance<TInterface, TImplementation>() where TImplementation : TInterface
        {
            this.m_Bindings.Add(typeof(TInterface), typeof(TImplementation));
            this.m_KeepInstance.Add(typeof(TInterface), true);
        }

        public T Get<T>()
        {
            return (T)this.Get(typeof(T), new List<Type>());
        }

        public T Get<T>(string name)
        {
            return (T)this.Get(this.m_NamedBindings[name], new List<Type>());
        }

        public object Get(Type t)
        {
            return this.Get(t, new List<Type>());
        }

        private object Get(Type original, List<Type> seen)
        {
            if (original == typeof(LightweightKernel))
            {
                return this;
            }

            if (seen.Contains(original))
            {
                throw new InvalidOperationException(
                    "Attempting to resolve " + 
                    original.FullName + 
                    ", but it has already been seen during resolution.");
            }

            seen.Add(original);

            Type actual;

            if (this.m_Bindings.ContainsKey(original))
            {
                if (this.m_KeepInstance[original])
                {
                    if (this.m_Instances.ContainsKey(original))
                    {
                        return this.m_Instances[original];
                    }
                }

                actual = this.m_Bindings[original];
            }
            else
            {
                actual = original;
            }

            if (actual.IsInterface || actual.IsAbstract)
            {
                throw new InvalidOperationException("Unable to resolve " + actual.FullName + " to a concrete implementation!");
            }

			ConstructorInfo constructor;
            var constructors = actual.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            if (constructors.Count == 0)
            {
                throw new InvalidOperationException("Tried to construct " + actual.FullName + ", but it has no constructors!");
            }
			else if (constructors.Count == 1)
			{
				constructor = constructors[0];
			}
			else
			{
				constructor = constructors.OrderBy(x => x.GetCustomAttribute<LightweightKernelInjectionPreferredAttribute>() != null ? 0 : 1).First();
			}

            var parameters = constructor.GetParameters();

            var resolved = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                resolved[i] = this.Get(parameterType, seen.ToList());
            }

            var instance = constructor.Invoke(resolved);

            if (this.m_Bindings.ContainsKey(original))
            {
                if (this.m_KeepInstance[original])
                {
                    if (!this.m_Instances.ContainsKey(original))
                    {
                        this.m_Instances[original] = instance;
                    }
                }
            }

            return instance;
        }
    }
}


﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Gofer.NET.Errors;
using StackExchange.Redis;

namespace Gofer.NET
{
    public class TaskInfo
    {
        public string Id { get; set; }
        
        public string AssemblyName { get; set; }
        
        public string TypeName { get; set; }
        
        public string MethodName { get; set; }

        public object[] Args { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }

        public bool IsExpired(TimeSpan expirationSpan)
        {
            return CreatedAtUtc < (DateTime.UtcNow - expirationSpan);
        }

        public void ExecuteTask()
        {
            var assembly = Assembly.Load(AssemblyName);
            var type = assembly.GetType(TypeName);
            
            var staticMethod = type.GetMethod(MethodName, 
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (staticMethod != null)
            {
                staticMethod.Invoke(null, Args);
                return;
            }
            
            var instanceMethod = type.GetMethod(MethodName, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (instanceMethod == null)
            {
                throw new UnableToDeserializeDelegateException();
            }

            var instance = Activator.CreateInstance(type);
            
            instanceMethod.Invoke(instance, Args);
        }
    }
}
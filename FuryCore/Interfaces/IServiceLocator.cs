namespace FuryCore.Interfaces;

using System;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public interface IServiceLocator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="TServiceType"></typeparam>
    /// <returns></returns>
    public Lazy<TServiceType> Lazy<TServiceType>(Action<TServiceType> action = default);

    /// <summary>
    /// 
    /// </summary>
    public void ForceEvaluation();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TServiceType"></typeparam>
    /// <returns></returns>
    public TServiceType FindService<TServiceType>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="exclude"></param>
    /// <returns></returns>
    public object FindService(Type type, IList<IServiceLocator> exclude);
}
﻿using System;

namespace PeanutButter.DuckTyping.Shimming
{
    /// <summary>
    /// Interface implemented by the TypeMaker
    /// </summary>
    public interface ITypeMaker
    {
        /// <summary>
        /// Attempts to create a type implementing interface T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction
        /// </summary>
        /// <typeparam name="T">Interface to implement</typeparam>
        /// <returns>A new type implementing interface T</returns>
        Type MakeTypeImplementing<T>();

        /// <summary>
        /// Attempts to create a type implementing interface T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction
        /// </summary>
        /// <param name="interfaceType">Interface to implement</param>
        /// <returns>A new type implementing interface T</returns>
        Type MakeTypeImplementing(Type interfaceType);

        /// <summary>
        /// Attempts to create a type implementing interface T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing)
        /// </summary>
        /// <typeparam name="T">Interface to implement</typeparam>
        /// <returns>A new type implementing interface T</returns>
        Type MakeFuzzyTypeImplementing<T>();

        /// <summary>
        /// Attempts to create a type implementing interface T
        /// The created type will have constructors supporting parameterless construction
        /// as well as duck-typing construction for approximate duck-typing (which also
        /// supports accurate duck-typing)
        /// </summary>
        /// <param name="interfaceType">Interface to implement</param>
        /// <returns>A new type implementing interface T</returns>
        Type MakeFuzzyTypeImplementing(Type interfaceType);
    }
}
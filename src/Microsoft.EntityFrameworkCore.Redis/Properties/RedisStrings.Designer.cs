﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.EntityFrameworkCore.Redis.Properties {
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using JetBrains.Annotations;
    
    
    /// <summary>
    ///   Classe de ressource fortement typée destinée, entre autres, à la recherche de chaînes localisées.
    /// </summary>
    // Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    // à l'aide d'un outil, tel que ResGen ou Visual Studio.
    // Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    // avec l'option /str ou régénérez votre projet VS.
    public class RedisStrings {
        
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.EntityFrameworkCore.Redis.Properties.RedisStrings", typeof(RedisStrings).GetTypeInfo().Assembly);

                /// <summary>
        ///    Recherche une chaîne localisée similaire à The string argument &apos;{argumentName}&apos; cannot be empty..
        /// </summary>
        public static string ArgumentIsEmpty {
            get {
                return GetString("ArgumentIsEmpty");
            }
        }
        
        /// <summary>
        ///    Recherche une chaîne localisée similaire à The bytes {0} could not be interpreted as a UTF-8 string..
        /// </summary>
        public static string InvalidDatabaseValue {
            get {
                return GetString("InvalidDatabaseValue");
            }
        }
        
        /// <summary>
        ///    Recherche une chaîne localisée similaire à The value provided for argument &apos;{argumentName}&apos; must be a valid value of enum type &apos;{enumType}&apos;..
        /// </summary>
        public static string InvalidEnumValue {
            get {
                return GetString("InvalidEnumValue");
            }
        }
        
        /// <summary>
        ///    Recherche une chaîne localisée similaire à Primary Key value for Entity &apos;{0}&apos;, Property &apos;{1}&apos; cannot be null..
        /// </summary>
        public static string InvalidPrimaryKeyValue {
            get {
                return GetString("InvalidPrimaryKeyValue");
            }
        }
        
        /// <summary>
        /// Saved {count} entities to in-memory store.
        /// </summary>
        public static string LogSavedChanges([CanBeNull] object count)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("LogSavedChanges", "count"), count);
        }
        
        /// <summary>
        /// Transactions are not supported by the in-memory store. See http://go.microsoft.com/fwlink/?LinkId=800142
        /// </summary>
        public static string TransactionsNotSupported
        {
            get { return GetString("TransactionsNotSupported"); }
        }

        /// <summary>
        /// Attempted to update or delete an entity that does not exist in the store.
        /// </summary>
        public static string UpdateConcurrencyException
        {
            get { return GetString("UpdateConcurrencyException"); }
        }
        
        /// <summary>
        ///    Recherche une chaîne localisée similaire à Cannot decode property of name &apos;{0}&apos; of type &apos;{1}&apos; on EntityType &apos;{2}&apos;..
        /// </summary>
        public static string UnableToDecodeProperty {
            get {
                return GetString("UnableToDecodeProperty");
            }
        }
        
        /// <summary>
        ///    Recherche une chaîne localisée similaire à Cannot update entity of type &apos;{0}&apos; with key &apos;{1}&apos; because the key does not exist in the Primary Key Index for that entity..
        /// </summary>
        public static string UnableToUpdate {
            get {
                return GetString("UnableToUpdate");
            }
        }

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);

            Debug.Assert(value != null);

            if (formatterNames != null)
            {
                for (var i = 0; i < formatterNames.Length; i++)
                {
                    value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
                }
            }

            return value;
        }
    }
}

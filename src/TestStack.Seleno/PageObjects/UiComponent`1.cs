using System;
using System.Linq.Expressions;
using System.Reflection;
using TestStack.Seleno.PageObjects.Actions;
using TestStack.Seleno.PageObjects.Controls;

namespace TestStack.Seleno.PageObjects
{
    public class UiComponent<TModel> : UiComponent
        where TModel : class, new()
    {
        public IPageReader<TModel> Read
        {
            get
            {
                return ComponentFactory.CreatePageReader<TModel>();
            }
        }

        public IPageWriter<TModel> Input
        {
            get
            {
                return ComponentFactory.CreatePageWriter<TModel>();
            }
        }

        public THtmlControl HtmlControlFor<THtmlControl>(Expression<Func<TModel, Object>> propertySelector,
            TimeSpan maxWait = default(TimeSpan))
            where THtmlControl : HTMLControl, new()
        {
            return ComponentFactory.HtmlControlFor<THtmlControl>(propertySelector, maxWait);
        }

        /// <summary>
        /// Simple helper to return the name of the property passed in as a lambda expression.
        /// This method helps prevent the need for magic strings and the declaration of the return type, TU,
        /// rather than "object" prevents errors with boxing value types.
        /// </summary>
        public string GetName<TU>(Expression<Func<TModel, TU>> propertyBlacklist)
        {
            var x = propertyBlacklist.Body as MemberExpression;

            if(x == null)
                throw new ArgumentException("Not a property or field");

            var y = x.Member as PropertyInfo;

            if(y == null)
                throw new ArgumentException("Could not access property or field");

            return y.Name;
        }
    }
}
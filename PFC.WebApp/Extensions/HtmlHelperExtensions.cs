using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using PFC.WebApp.Support;

namespace Microsoft.AspNetCore.Mvc
{
    public static class HtmlHelperExtensions
    {

        private class MvcGenericClosure : IDisposable
        {
            private readonly IHtmlHelper htmlHelper;
            private readonly TagBuilder tagBuilder;

            public MvcGenericClosure(IHtmlHelper htmlHelper, string tag, object htmlAttributes = null)
            {
                this.htmlHelper = htmlHelper;

                tagBuilder = new TagBuilder(tag);
                foreach (var attr in HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes).Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())))
                    tagBuilder.Attributes.Add(attr);

                tagBuilder.RenderStartTag().WriteTo(htmlHelper.ViewContext.Writer, HtmlEncoder.Default);
            }

            public void Dispose()
            {
                tagBuilder.RenderEndTag().WriteTo(htmlHelper.ViewContext.Writer, HtmlEncoder.Default);
            }
        }

        class DisposableWraper : IDisposable
        {
            private readonly IDisposable iDisposable;
            Action actionBeforeDispose;

            public DisposableWraper(IDisposable iDisposable)
            {
                this.iDisposable = iDisposable;
            }

            public void Dispose()
            {
                actionBeforeDispose?.Invoke();
                iDisposable.Dispose();
            }

            internal IDisposable WithBeforeDispose(Action action)
            {
                actionBeforeDispose = action;
                return this;
            }
        }

        public static IDisposable BeginCustomForm(this IHtmlHelper htmlHelper)
        {
            var form = htmlHelper.ViewData.ContainsKey("FormEditionDisabled")
                ? BeginGenericClosure(htmlHelper)
                : htmlHelper.BeginForm();

            if (htmlHelper.ViewData.ContainsKey("FormEditionDisabled"))
                htmlHelper.ViewContext.Writer.Write($"<div class='alert alert-warning' role='alert'>El formulario ha sido dehabilitado por la siguiente razón: { htmlHelper.ViewData["FormEditionDisabled"] }</div>");

            if (!htmlHelper.ViewData.ModelState.IsValid)
                htmlHelper.ValidationSummary("", new { @class = "alert alert-danger" }).WriteTo(htmlHelper.ViewContext.Writer, HtmlEncoder.Default);

            htmlHelper.ViewContext.Writer.Write("<div class='row'><div class='col-md-4'>");


            return new DisposableWraper(form).WithBeforeDispose(() =>
            {
                htmlHelper.ViewContext.Writer.Write("</div></div>");
            });
        }



        public static IDisposable BeginGenericClosure(this IHtmlHelper htmlHelper, string tag = "div", object htmlAttributes = null)
        {
            return new MvcGenericClosure(htmlHelper, tag, htmlAttributes);
        }

        public static IHtmlContent DataTableForModel<T>(this IHtmlHelper<T> htmlHelper)
        {
            return htmlHelper.Partial("~/Views/Shared/DataTableDefinition.cshtml");
        }

        public static IHtmlContent DataTableFor(this IHtmlHelper htmlHelper, DataTableDefinition model)
        {
            return htmlHelper.Partial("~/Views/Shared/DataTableDefinition.cshtml", model);
        }

        /// <summary>
        /// Inserta un campo con label para formulario
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="html"></param>
        /// <param name="expression"></param>
        /// <param name="type"></param>
        /// <param name="placeholder"></param>
        /// <param name="display"></param>
        /// <returns></returns>
        public static IHtmlContent FormFieldFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression,
            object editorHtmlAttributes = null,
            string displayName = null,
            bool? required = null,
            bool editable = true,
            IEnumerable<SelectListItem> selectList = null)
        {
            html.Html5DateRenderingMode = Html5DateRenderingMode.Rfc3339;

            var metadata = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider).Metadata;

            if (!required.HasValue)
                required = metadata.IsRequired;

            var label = html.LabelFor(expression, htmlAttributes: new { @class = "control-label" });

            var name = html.NameFor(expression).ToString();

            var hasError = (html.ViewData.ModelState.Any(x => x.Key == name) && html.ViewData.ModelState.Single(x => x.Key == name).Value.Errors.Count > 0)
                ? " has-error" : "";


            IDictionary<string, object> htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(editorHtmlAttributes);
            if (!htmlAttributes.ContainsKey("class"))
                htmlAttributes["class"] = "";

            IHtmlContent editor;
            IHtmlContent validation = null;

            if (editable && !html.ViewData.ContainsKey("FormEditionDisabled"))
            {
                htmlAttributes["class"] = htmlAttributes["class"] + " form-control";
                htmlAttributes["placeholder"] = metadata.Placeholder;

                editor = selectList == null
                    ? html.EditorFor(expression, new { htmlAttributes })
                    : html.DropDownListFor(expression, selectList, htmlAttributes);

                validation = html.ValidationMessageFor(expression, "", new { @class = "text-danger help-block" });
            }
            else
            {
                htmlAttributes["class"] = htmlAttributes["class"] + " form-control-static";
                var tagBuilder = new TagBuilder("p");
                tagBuilder.MergeAttributes(htmlAttributes);

                editor = new HtmlContentBuilder()
                    .AppendHtml(tagBuilder.RenderStartTag())
                    .AppendHtml(html.DisplayFor(expression))
                    .AppendHtml(tagBuilder.RenderEndTag());
            }


            IHtmlContentBuilder htmlContentBuilder = new HtmlContentBuilder()
                .AppendHtml($@"<div class='form-group{hasError}'>")
                .AppendHtml(required.Value ? "<i class='glyphicon glyphicon-asterisk'></i>" : "")
                .AppendHtml(label)
                .AppendHtml("<div>")
                .AppendHtml(editor);

            if (validation != null)
                htmlContentBuilder = htmlContentBuilder.AppendHtml(validation);

            return htmlContentBuilder.AppendHtml("</div></div>");
        }

        public static IHtmlContent SubmitButton(this IHtmlHelper htmlHelper, string text = "Guardar")
        {
            var editable = !htmlHelper.ViewData.ContainsKey("FormEditionDisabled");
            return new HtmlContentBuilder().AppendHtml($"<input type='submit' value='{ text }' class='btn btn-primary{(editable ? "" : " disabled")}'{ (editable ? "" : " disabled") } />");
        }

        public static IHtmlContent CancelButton(this IHtmlHelper htmlHelper, string url, string text = "Volver")
        {
            return new HtmlContentBuilder().AppendHtml($"<a href='{ url }' class='btn btn-default' />{ text }</a>");
        }
    }
}

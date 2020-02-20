﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstateReporting.Common
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.SwaggerGen;

    [ExcludeFromCodeCoverage]
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        #region Fields

        /// <summary>
        /// The provider
        /// </summary>
        private readonly IApiVersionDescriptionProvider provider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            // add a swagger document for each discovered API version
            // note: you might choose to skip or document deprecated API versions differently
            foreach (ApiVersionDescription description in this.provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, ConfigureSwaggerOptions.CreateInfoForApiVersion(description));
            }
        }

        /// <summary>
        /// Creates the information for API version.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            OpenApiInfo info = new OpenApiInfo
            {
                Title = "Golf Handicapping API",
                Version = description.ApiVersion.ToString(),
                Description = "A REST Api to manage the golf club handicapping system.",
                Contact = new OpenApiContact
                {
                    Name = "Stuart Ferguson",
                    Email = "golfhandicapping@btinternet.com"
                },
                License = new OpenApiLicense
                {
                    Name = "TODO"
                }
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        #endregion
    }

    [ExcludeFromCodeCoverage]
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation,
                          OperationFilterContext context)
        {
            ApiDescription apiDescription = context.ApiDescription;
            ApiVersion apiVersion = apiDescription.GetApiVersion();
            ApiVersionModel model = apiDescription.ActionDescriptor.GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

            operation.Deprecated = model.DeprecatedApiVersions.Contains(apiVersion);

            if (operation.Parameters == null)
            {
                return;
            }

            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                ApiParameterDescription description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class SwaggerJsonConverter : JsonConverter
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.
        /// </value>
        public override Boolean CanRead => false;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public override Object ReadJson(JsonReader reader,
                                        Type objectType,
                                        Object existingValue,
                                        JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer,
                                       Object value,
                                       JsonSerializer serializer)
        {
            // Disable sending the $type in the serialized json
            serializer.TypeNameHandling = TypeNameHandling.None;

            JToken t = JToken.FromObject(value);
            t.WriteTo(writer);
        }

        #endregion
    }
}
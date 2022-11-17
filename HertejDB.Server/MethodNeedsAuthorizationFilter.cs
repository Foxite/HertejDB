using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HertejDB.Server;

/// <summary>
/// Provides a method that applies Swagger UI security requirements to all controller actions that need authorization.
/// </summary>
/// <remarks>
/// https://stackoverflow.com/a/72204632
/// </remarks>
public class MethodNeedsAuthorizationFilter : IOperationFilter {
	public void Apply(OpenApiOperation operation, OperationFilterContext context) {
		if (operation is null) throw new ArgumentNullException(nameof(operation));
		if (context is null) throw new ArgumentNullException(nameof(context));

		object[] methodAttributes = context.MethodInfo.GetCustomAttributes(true);

		bool needsAuth =
			methodAttributes.OfType<AuthorizeAttribute>().Any()
			|| (context.MethodInfo.DeclaringType != null
			    && context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
			    && !methodAttributes.OfType<AllowAnonymousAttribute>().Any());

		if (needsAuth) {
			OpenApiSecurityScheme scheme = new() {
				Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "OpenID"},
				In = ParameterLocation.Header,
			};

			operation.Security = new List<OpenApiSecurityRequirement>() {
				new() {{scheme, new List<string>()}}
			};
		}
	}
}

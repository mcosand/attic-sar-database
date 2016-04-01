/*
 * Copyright Matthew Cosand
  */
namespace Sar.Auth
{
  using System.Net;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Web.Http;
  using System.Web.Http.ExceptionHandling;

  public class ApiUserExceptionHandler : IExceptionHandler
  {
    private readonly IExceptionHandler _next;

    public ApiUserExceptionHandler(IExceptionHandler next)
    {
      _next = next;
    }

    public Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
    {
      var exception = context.Exception as UserErrorException;
      if (exception == null)
      {
        return _next.HandleAsync(context, cancellationToken);
      }
      else
      {
        context.Result = new TextPlainErrorResult { Request = context.ExceptionContext.Request, Content = exception.ExternalMessage };
        return Task.FromResult(0);
      }
    }

    private class TextPlainErrorResult : IHttpActionResult
    {
      public HttpRequestMessage Request { get; set; }

      public string Content { get; set; }

      public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
      {
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        response.Content = new StringContent(Content);
        response.RequestMessage = Request;
        return Task.FromResult(response);
      }
    }
  }
}
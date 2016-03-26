/*
 * Copyright Matthew Cosand
 */
using System;

namespace Sar
{
  public class MultipleMatchesException : ApplicationException
  {
    public MultipleMatchesException() : base() { }
    public MultipleMatchesException(string message) : base(message) { }
  }
}

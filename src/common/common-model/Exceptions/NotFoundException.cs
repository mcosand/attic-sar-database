/*
 * Copyright Matthew Cosand
 */
using System;

namespace Sar
{
  public class NotFoundException : ApplicationException
  {
    public NotFoundException() : base() { }
    public NotFoundException(string message) : base(message) { }
  }
}

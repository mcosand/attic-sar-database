/*
 * Copyright Matthew Cosand
 */
using System;

namespace Sar.Database.Model
{
  public class NameIdPair<T>
  {
    public T Id { get; set; }
    public string Name { get; set; }
  }

  public class NameIdPair : NameIdPair<Guid>
  {

  }
}
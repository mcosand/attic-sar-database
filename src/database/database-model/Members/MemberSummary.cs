/*
 * Copyright Matthew Cosand
 */
using System;

namespace Sar.Database.Model.Members
{
  public class MemberSummary
  {
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string WorkerNumber { get; set; }

    public string Photo { get; set; }

    public NameIdPair[] Units { get; set; }
  }
}

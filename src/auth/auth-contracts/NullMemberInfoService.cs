/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sar.Auth
{
  public class NullMemberInfoService : IMemberInfoService
  {
    public Task<IList<Member>> FindMembersByEmail(string email)
    {
      return Task.FromResult((IList<Member>)new Member[0]);
    }

    public Task<Member> GetMember(Guid memberid)
    {
      return Task.FromResult((Member)null);
    }
  }
}

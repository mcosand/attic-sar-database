/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sar.Auth
{
  public interface IMemberInfoService
  {
    Task<IList<Member>> FindMembersByEmail(string email);
    Task<Member> GetMember(Guid memberId);
  }
}

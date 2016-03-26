using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using System.Data.Entity;
using Sar.Auth;

namespace Sar.Database.Services
{
  public class MembersService : Auth.IMemberInfoService
  {
    private readonly Func<IKcsarContext> _dbFactory;

    public MembersService(Func<IKcsarContext> dbFactory)
    {
      _dbFactory = dbFactory;
    }

    async Task<Auth.Member> IMemberInfoService.GetMember(Guid memberId)
    {
      using (var db = _dbFactory())
      {
        DateTime now = DateTime.Now;
        return await db.Members.Where(f => f.Id == memberId)
          .Select(f => new Auth.Member
          {
            Id = f.Id,
            FirstName = f.FirstName,
            LastName = f.LastName,
            Email = f.ContactNumbers.Where(g => g.Type == "email").OrderByDescending(g => g.Priority).Select(g => g.Value).FirstOrDefault(),
            Units = f.Memberships
                   .Where(g => g.Activated < now && (g.EndTime == null || g.EndTime > now) && g.Status.IsActive)
                   .Select(g => new Auth.Organization
                   {
                     Id = g.Unit.Id,
                     Name = g.Unit.DisplayName,
                     LongName = g.Unit.LongName
                   })
          }).FirstOrDefaultAsync();
      }
    }

    async Task<IList<Auth.Member>> IMemberInfoService.FindMembersByEmail(string email)
    {
      using (var db = _dbFactory())
      {
        DateTime now = DateTime.Now;

        var members = await db.PersonContact.Where(f => f.Value == email && f.Type == "email").Select(f => new Auth.Member
        {
          Id = f.Person.Id,
          FirstName = f.Person.FirstName,
          LastName = f.Person.LastName,
          Email = f.Value,
          Units = f.Person.Memberships
                   .Where(g => g.Activated < now && (g.EndTime == null || g.EndTime > now) && g.Status.IsActive)
                   .Select(g => new Auth.Organization
                   {
                     Id = g.Unit.Id,
                     Name = g.Unit.DisplayName,
                     LongName = g.Unit.LongName
                   })
        }).ToListAsync();

        return members;
      }
    }
  }
}

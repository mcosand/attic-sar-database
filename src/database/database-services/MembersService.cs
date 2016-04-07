/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sar.Auth;
using Sar.Database.Model;
using Sar.Database.Model.Members;
using DbModel = Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public interface IMembersService
  {
    Task<IEnumerable<MemberSummary>> ByPhoneNumber(string id);
    Task<IEnumerable<MemberSummary>> ByWorkerNumber(string id);
  }

  public class MembersService : IMembersService, Auth.IMemberInfoService
  {
    private readonly Func<DbModel.IKcsarContext> _dbFactory;

    public MembersService(Func<DbModel.IKcsarContext> dbFactory)
    {
      _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<MemberSummary>> ByWorkerNumber(string id)
    {
      id = id.TrimStart('S', 'R');

      using (var db = _dbFactory())
      {
        return await SummariesWithUnits(db.Members.Where(f => f.DEM == id || f.DEM == "SR" + id));
      }
    }

    public async Task<IEnumerable<MemberSummary>> ByPhoneNumber(string id)
    {
      if (id.Length < 10 || !Regex.IsMatch(id, "\\d+"))
      {
        return new MemberSummary[0];
      }

      var pattern = string.Format("%{0}%{1}%{2}%",
        id.Substring(id.Length - 10, 3),
        id.Substring(id.Length - 7, 3),
        id.Substring(id.Length - 4, 4));

      using (var db = _dbFactory())
      {
        return await SummariesWithUnits(db.Members.Where(f => f.ContactNumbers.Any(g => SqlFunctions.PatIndex(pattern, g.Value) > 0)));
      }
    }

    internal static async Task<IEnumerable<MemberSummary>> SummariesWithUnits(IQueryable<DbModel.Member> query)
    {
      DateTime cutoff = DateTime.Now;
      return (await query
        .Select(f => new
        {
          Member = f,
          Units = f.Memberships
                   .Where(g => (g.EndTime == null || g.EndTime > cutoff) && g.Status.IsActive)
                   .Select(g => g.Unit)
                   .Select(g => new NameIdPair
                   {
                     Id = g.Id,
                     Name = g.DisplayName
                   }).Distinct()
        })
        .OrderBy(f => f.Member.LastName).ThenBy(f => f.Member.FirstName)
        .ToListAsync())
        .Select(f => new MemberSummary
        {
          Name = f.Member.FullName,
          WorkerNumber = f.Member.DEM,
          Id = f.Member.Id,
          Units = f.Units.ToArray(),
          Photo = f.Member.PhotoFile
        });
    }

    #region Auth.IMemberInfoService

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
      #endregion
    }
  }
}

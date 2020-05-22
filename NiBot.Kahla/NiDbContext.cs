using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NiBot.Kahla.Models;

namespace NiBot.Kahla
{
    public class NiDbContext: DbContext
    {
        public DbSet<NiBind> Binds { get; set; }

        public NiDbContext(DbContextOptions options) : base(options) { }

        public async Task<List<NiBind>> GetBinds(int conversationId)
        {
            return await Binds
                .Where(t => t.ConversationId == conversationId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

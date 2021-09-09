using System;
using System.Collections.Generic;
using System.Linq;
using CommandsService.Models;

namespace CommandsService.Data
{
    public class CommandRepository : ICommandRepository
    {
        private readonly AppDbContext _dbContext;

        public CommandRepository(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public void CreateCommand(int platformId, Command command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            command.PlatformId = platformId;

            this._dbContext.Commands.Add(command);
        }

        public void CreatePlatform(Platform platform)
        {
            if (platform == null)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            this._dbContext.Platforms.Add(platform);
        }

        public IEnumerable<Platform> GetAllPlatforms()
        {
            return this._dbContext
                .Platforms
                .ToList();
        }

        public Command GetCommand(int platformId, int commandId)
        {
            return this._dbContext
                .Commands
                .Where(c => c.PlatformId == platformId && c.Id == commandId)
                .FirstOrDefault();
        }

        public IEnumerable<Command> GetCommandsForPlatform(int platformId)
        {
            return this._dbContext
                .Commands
                .Where(c => c.PlatformId == platformId)
                .OrderBy(c => c.Platform.Name);
        }

        public bool PlatformExists(int platformId)
        {
            return this._dbContext.Platforms.Any(p => p.Id == platformId);
        }

        public bool ExternalPlatformExists(int platformId)
        {
            return this._dbContext.Platforms.Any(p => p.ExternalId == platformId);
        }

        public bool SaveChanges()
        {
            return (this._dbContext.SaveChanges() >= 0);
        }
    }
}
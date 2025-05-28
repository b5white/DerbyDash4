using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash.TempModels;

public partial class AspnetDerbyDash46223fa300c946c8987586095fc1a6a5Context : DbContext
{
    public AspnetDerbyDash46223fa300c946c8987586095fc1a6a5Context()
    {
    }

    public AspnetDerbyDash46223fa300c946c8987586095fc1a6a5Context(DbContextOptions<AspnetDerbyDash46223fa300c946c8987586095fc1a6a5Context> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

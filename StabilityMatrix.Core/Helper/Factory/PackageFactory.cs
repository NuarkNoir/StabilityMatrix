﻿using StabilityMatrix.Core.Models.Packages;

namespace StabilityMatrix.Core.Helper.Factory;

public class PackageFactory : IPackageFactory
{
    /// <summary>
    /// Mapping of package.Name to package
    /// </summary>
    private readonly Dictionary<string, BasePackage> basePackages;

    public PackageFactory(IEnumerable<BasePackage> basePackages)
    {
        this.basePackages = basePackages.ToDictionary(x => x.Name);
    }
    
    public IEnumerable<BasePackage> GetAllAvailablePackages()
    {
        return basePackages.Values;
    }

    public BasePackage? FindPackageByName(string? packageName)
    {
        return packageName == null ? null : 
            basePackages.GetValueOrDefault(packageName);
    }
}

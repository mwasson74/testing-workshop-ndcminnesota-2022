// <copyright file="IClientRepository.cs" company="Eurofins Scientific, Inc.">
//    Copyright © 2022 Eurofins Scientific, Inc. All rights reserved.
// </copyright>

namespace LegacyApp;

public interface IClientRepository
{
  Client GetById(int id);
}

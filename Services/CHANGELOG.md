# Changelog

All notable changes to this package are documented in this file.

## [0.1.0] - 2026-09-13

### Added

- IStorageService interface with methods for file existence check, file retrieval, reading files, and saving files across different storage mediums.
  - Azure Blob Storage
  - OS File System

- ICacheService interface with methods for caching data, retrieving cached data, and clearing cache.
  - Azure Blob Storage implementation of ICacheService.
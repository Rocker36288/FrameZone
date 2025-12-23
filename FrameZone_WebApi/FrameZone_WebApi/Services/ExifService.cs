using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;

namespace FrameZone_WebApi.Services
{
    public class ExifService : IExifService
    {
        private readonly ILogger<ExifService> _logger;

        public ExifService(ILogger<ExifService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 從圖片串流中提取 EXIF 元數據
        /// </summary>
        public PhotoMetadataDTO ExtractMetadata(Stream imageStream, string fileName)
        {
            var metadata = new PhotoMetadataDTO
            {
                FileName = Path.GetFileNameWithoutExtension(fileName),
                FileExtension = Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.'),
                FileSize = imageStream.Length,
            };

            try
            {
                // 重置串流位置
                imageStream.Position = 0;

                // 使用 MetadataExtractor 讀取所有 metadata
                var directories = ImageMetadataReader.ReadMetadata(imageStream);

                // 提取 EXIF 資訊
                ExtractExifData(directories, metadata);

                // 提取 GPS 資訊
                ExtractGpsData(directories, metadata);

                // 提取圖片尺寸
                ExtractImageDimensions(directories, metadata);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提取 EXIF 元數據時發生錯誤，檔案: {FileName}", fileName);
            }

            return metadata;
        }

        /// <summary>
        /// 計算檔案 SHA256 Hash
        /// </summary>
        public string CalculateFileHash(Stream fileStream)
        {
            fileStream.Position = 0;
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(fileStream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// 根據 EXIF 資訊和地址資訊自動生成分類標籤
        /// </summary>
        public List<string> GenerateAutoTags(PhotoMetadataDTO metadata, AddressInfoDTO addressInfo = null)
        {
            var tags = new List<string>();

            // 時間分類標籤
            if (metadata.DateTaken.HasValue)
            {
                var date = metadata.DateTaken.Value;
                tags.Add($"{date.Year}");
            }

            // 相機分類標籤
            if (!string.IsNullOrEmpty(metadata.CameraMake))
            {
                tags.Add(metadata.CameraMake.Trim());
            }

            // 地點標籤（如果有地址資訊）
            if (addressInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(addressInfo.Country))
                {
                    tags.Add(addressInfo.Country.Trim());
                }

                if (!string.IsNullOrWhiteSpace(addressInfo.City))
                {
                    tags.Add(addressInfo.City.Trim());
                }

                if (!string.IsNullOrWhiteSpace(addressInfo.District))
                {
                    tags.Add(addressInfo.District.Trim());
                }

                if (!string.IsNullOrWhiteSpace(addressInfo.PlaceName))
                {
                    tags.Add(addressInfo.PlaceName.Trim());
                }
            }

            return tags;
        }

        #region 私有方法

        /// <summary>
        /// 提取基本 EXIF 資訊
        /// </summary>
        private void ExtractExifData(IEnumerable<MetadataExtractor.Directory> directories, PhotoMetadataDTO metadata)
        {
            var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

            if (exifSubIfdDirectory != null)
            {
                // 拍攝時間
                if (exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateTaken))
                    metadata.DateTaken = dateTaken;

                // ISO
                if (exifSubIfdDirectory.TryGetInt32(ExifDirectoryBase.TagIsoEquivalent, out var iso))
                {
                    metadata.ISO = iso;
                }

                // 光圈
                if (exifSubIfdDirectory.TryGetDouble(ExifDirectoryBase.TagAperture, out var aperture))
                {
                    metadata.Aperture = (decimal)Math.Round(aperture, 1);
                }

                // 快門速度
                var shutterSpeed = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagShutterSpeed);
                if (!string.IsNullOrEmpty(shutterSpeed))
                {
                    metadata.ShutterSpeed = shutterSpeed;
                }

                // 焦距
                if (exifSubIfdDirectory.TryGetDouble(ExifDirectoryBase.TagFocalLength, out var focalLength))
                {
                    metadata.FocalLength = (decimal)Math.Round(focalLength, 1);
                }

                // 曝光模式
                metadata.ExposureMode = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagExposureMode);

                // 白平衡
                metadata.WhiteBalance = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagWhiteBalance);

                // 鏡頭型號
                metadata.LensModel = exifSubIfdDirectory.GetDescription(ExifDirectoryBase.TagLensModel);
            }

            if (exifIfd0Directory != null)
            {
                // 相機品牌
                metadata.CameraMake = exifIfd0Directory.GetDescription(ExifDirectoryBase.TagMake);

                // 相機型號
                metadata.CameraModel = exifIfd0Directory.GetDescription(ExifDirectoryBase.TagModel);

                // 方向
                if (exifIfd0Directory.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orientation))
                {
                    metadata.Orientation = orientation;
                }
            }
        }

        /// <summary>
        /// 提取 GPS 資訊
        /// </summary>
        private void ExtractGpsData(IEnumerable<MetadataExtractor.Directory> directories, PhotoMetadataDTO metadata)
        {
            var gpsDirectory = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();

            if (gpsDirectory != null)
            {
                try
                {
                    var location = gpsDirectory.GetGeoLocation();

                    if (location != null)
                    {
                        metadata.GPSLatitude = (decimal)location.Latitude;
                        metadata.GPSLongitude = (decimal)location.Longitude;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("無法提取 GPS 資訊: {Message}", ex.Message);
                }
            }
        }

        /// <summary>
        /// 提取圖片尺寸
        /// </summary>
        private void ExtractImageDimensions(IEnumerable<MetadataExtractor.Directory> directories, PhotoMetadataDTO metadata)
        {
            var jpegDirectory = directories.OfType<JpegDirectory>().FirstOrDefault();
            if (jpegDirectory != null)
            {
                if (jpegDirectory.TryGetInt32(JpegDirectory.TagImageWidth, out var width))
                {
                    metadata.Width = width;
                }
                if (jpegDirectory.TryGetInt32(JpegDirectory.TagImageHeight, out var height))
                {
                    metadata.Height = height;
                }
            }
        }

        #endregion
    }
}
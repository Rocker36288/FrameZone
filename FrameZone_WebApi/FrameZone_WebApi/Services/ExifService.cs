using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    public class ExifService : IExifService
    {
        /// <summary>
        /// 從圖片串流中提取 EXIF 元數據
        /// </summary>
        public PhotoMetadataDtos ExtractMetadata(Stream imageStream, string fileName)
        {
            var metadata = new PhotoMetadataDtos
            {
                FileName = Path.GetFileNameWithoutExtension(fileName),
                FileExtension = Path.GetExtension(fileName).ToLowerInvariant().TrimStart('.'),
                FileSize = imageStream.Length,
            };

            try
            {
                // 重製串流位置
                imageStream.Position = 0;

                // 計算檔案 Hash
                metadata.Hash = CalculateFileHash(imageStream);

                // 重置串流位置以讀取 EXIF
                imageStream.Position = 0;

                // 使用 MetadataExtractor 讀取所有 metadata
                var directories = ImageMetadataReader.ReadMetadata(imageStream);

                // 提取 EXIF 資訊
                ExtractExifData(directories, metadata);

                // 提取 GPS 資訊
                ExtractGpsData(directories, metadata);

                // 提取圖片尺寸
                ExtractImageDimensions(directories, metadata);

                // 自動生成標籤
                metadata.AutoTags = GenerateAutoTags(metadata);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting metdata: {ex.Message}");
            }

            return metadata;
        }

        /// <summary>
        /// 計算檔案 SHA256 Hash
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
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
        /// 提取基本 EXIF 資訊
        /// </summary>
        private void ExtractExifData(IEnumerable<MetadataExtractor.Directory> directories, PhotoMetadataDtos metadata)
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
        private void ExtractGpsData(IEnumerable<MetadataExtractor.Directory> directories, PhotoMetadataDtos metadata)
        {
            var gpsDirectory = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();

            if (gpsDirectory != null)
            {
                if (gpsDirectory.TryGetGeoLocation(out var location))
                {
                    metadata.GPSLatitude = (decimal)location.Latitude;
                    metadata.GPSLongitude = (decimal)location.Longitude;
                }
            }
        }

        /// <summary>
        /// 提取圖片尺寸
        /// </summary>
        private void ExtractImageDimensions(IEnumerable<MetadataExtractor.Directory> directories, PhotoMetadataDtos metadata)
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

        /// <summary>
        /// 根據 EXIF 標籤自動生成分類標籤
        /// </summary>
        public List<string> GenerateAutoTags(PhotoMetadataDtos metadata)
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

            // TODO: 地點標籤（之後加入反向地理編碼）
            if (metadata.GPSLatitude.HasValue && metadata.GPSLongitude.HasValue)
            {
                tags.Add("有GPS");
            }

            return tags;
        }

        #region Helper Methods

        private string GetSeason(int month)
        {
            return month switch
            {
                12 or 1 or 2 => "冬天",
                3 or 4 or 5 => "春天",
                6 or 7 or 8 => "夏天",
                9 or 10 or 11 => "秋天",
                _ => "未知"
            };
        }

        #endregion

    }
}

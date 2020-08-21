﻿using AMDownloader.ObjectModel;
using AMDownloader.RequestThrottling.Model;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AMDownloader.RequestThrottling
{
    class RequestThrottler
    {
        ConcurrentQueue<RequestModel> _requestList;
        CancellationTokenSource _cts;
        CancellationToken _ct;
        int _interval;

        public RequestThrottler(int interval)
        {
            _requestList = new ConcurrentQueue<RequestModel>();
            _interval = interval;
        }

        public RequestModel? Has(string value)
        {
            if (_requestList.IsEmpty) return null;
            var items = (from item in _requestList where item.Url == value select item).ToArray();
            if (items.Count() > 0)
            {
                return items[0];
            }
            else
            {
                return null;
            }
        }

        public void Keep(string value, long? totalBytesToDownload = null, HttpStatusCode? status = null)
        {
            RequestModel requestModel;
            requestModel.Url = value;
            requestModel.SeenAt = DateTime.Now;
            requestModel.TotalBytesToDownload = totalBytesToDownload;
            requestModel.StatusCode = status;
            _requestList.Enqueue(requestModel);
            if (_cts == null)
            {
                Task.Run(async () => await RemoveUrls());
            }
        }

        private async Task RemoveUrls()
        {
            _cts = new CancellationTokenSource();
            _ct = _cts.Token;

            while (!_requestList.IsEmpty)
            {
                if (_ct.IsCancellationRequested)
                {
                    break;
                }
                if (!_requestList.TryDequeue(out RequestModel requestModel))
                {
                    continue;
                }
                TimeSpan diff = DateTime.Now.Subtract(requestModel.SeenAt);
                if (diff.TotalSeconds < 60)
                {
                    int delay = 60000 - (int)diff.TotalMilliseconds;
                    await Task.Delay(delay);
                }
            }
            _cts = null;
            _ct = default;
        }
    }
}
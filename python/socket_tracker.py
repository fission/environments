import os

import gevent
import requests

WS_START_EVENT = "http://127.0.0.1:8000/wsevent/start"
WS_END_EVENT = "http://127.0.0.1:8000/wsevent/end"

TIMEOUT = int(os.environ.get("TIMEOUT", 60))
MOCK_FETCHER = os.environ.get("MOCK_FETCHER", "false") == "true"


class SocketTrackerException(Exception):
    pass


class WebsocketTracker(object):

    def __init__(self, logger):
        self.logger = logger
        self.is_started = False
        self.clients = []
        self.logger.info("Websocket tracker initialized")
        self._monitor_duration = TIMEOUT
        self._run_monitor = False

    def _remove_stale_clients(self):
        count = len(self.clients)
        active_clients = []
        for c in self.clients:
            self.logger.info("Checking client {}".format(c.__dict__))
            if not c.closed:
                active_clients.append(c)
        self.clients = active_clients
        if count > len(self.clients):
            self.logger.info(
                "Cleaned up connections: {}".format(count - len(self.clients)))

    def _active_connection_event(self):
        if not MOCK_FETCHER:
            resp = requests.get(WS_START_EVENT)
            if resp.status_code != 200:
                raise SocketTrackerException(
                    "Failed to start websocket tracker")
        self.logger.info("Recorded active connection event")

    def _no_activity_event(self):
        if not MOCK_FETCHER:
            resp = requests.get(WS_END_EVENT)
            if resp.status_code != 200:
                raise SocketTrackerException("Failed to end websocket")
        self.logger.info("Recorded no activity event")

    def _monitor(self):
        if len(self.clients) == 0:
            return
        self.logger.info("Running monitor for websockets")
        self._remove_stale_clients()
        if len(self.clients) == 0:
            self._no_activity_event()
            self.is_started = False
            return

    def monitor(self):
        self._run_monitor = True
        while self._run_monitor:
            gevent.sleep(self._monitor_duration)
            self._monitor()

    def stop_monitor(self):
        self._run_monitor = False

    def add_client(self, client):
        if not self.is_started:
            self._active_connection_event()
            self.is_started = True
        self.clients.append(client)

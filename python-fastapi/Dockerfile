ARG PY_BASE_IMG=3.11-alpine

FROM python:${PY_BASE_IMG}

WORKDIR /app

RUN pip install --no-cache-dir --upgrade pip

COPY requirements.txt /app
RUN pip install --no-cache-dir --upgrade -r requirements.txt

COPY *.py /app

ENTRYPOINT ["python3"]
CMD ["server.py"]

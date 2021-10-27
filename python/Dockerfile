ARG PY_BASE_IMG

FROM python:${PY_BASE_IMG}

WORKDIR /app

RUN apk add --update gcc && \
    apk upgrade --no-cache && \
    apk add --no-cache python3-dev build-base libev-dev libffi-dev bash && \
    rm -r /root/.cache

COPY requirements.txt /app
RUN pip3 install -r requirements.txt

COPY . /app

ENV PYTHONUNBUFFERED 1
ENTRYPOINT ["python3"]
CMD ["server.py"]

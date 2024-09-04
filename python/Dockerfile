ARG PY_BASE_IMG=3.11-alpine

FROM python:${PY_BASE_IMG}

WORKDIR /app

RUN apk add --update --no-cache gcc python3-dev build-base libev-dev libffi-dev bash

COPY requirements.txt /app
RUN pip3 install -r requirements.txt

COPY *.py /app/

ENV PYTHONUNBUFFERED=1
ENTRYPOINT ["python3"]
CMD ["server.py"]
